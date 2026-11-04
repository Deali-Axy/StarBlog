import importlib.util
from pathlib import Path
import unittest
import json
import tempfile
from unittest.mock import patch

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location('publish_deployment', ROOT / 'deploy/publish-deployment.py')
publish = importlib.util.module_from_spec(spec)
spec.loader.exec_module(publish)


class ImageRecipeTests(unittest.TestCase):
    def manifest(self):
        return {'version': 'v1', 'artifacts': [
            {'type': 'container-image', 'profile': profile, 'ref': publish.REGISTRY + ':v1-' + profile, 'digest': 'sha256:' + digit * 64}
            for profile, digit in [('api', 'a'), ('web', 'b'), ('gateway', 'c')]
        ]}

    def test_all_three_images_are_pinned_by_digest(self):
        pins = publish.image_pins(self.manifest(), 'v1')
        self.assertEqual(set(pins), {'API_IMAGE', 'WEB_IMAGE', 'GATEWAY_IMAGE'})
        self.assertTrue(all('@sha256:' in pin for pin in pins.values()))

    def test_incomplete_or_unpublished_release_is_rejected(self):
        manifest = self.manifest()
        manifest['artifacts'].pop()
        with self.assertRaisesRegex(ValueError, 'complete'):
            publish.image_pins(manifest, 'v1')
        manifest = self.manifest()
        manifest['artifacts'][0]['digest'] = ''
        with self.assertRaisesRegex(ValueError, 'pinned'):
            publish.image_pins(manifest, 'v1')

    def test_wrong_release_or_registry_is_rejected(self):
        with self.assertRaisesRegex(ValueError, 'version mismatch'):
            publish.image_pins(self.manifest(), 'v2')
        manifest = self.manifest()
        manifest['artifacts'][0]['ref'] = 'untrusted.example/api:v1'
        with self.assertRaisesRegex(ValueError, 'complete'):
            publish.image_pins(manifest, 'v1')

    def test_conflicting_published_digest_is_rejected(self):
        manifest = self.manifest()
        manifest['artifacts'].append({**manifest['artifacts'][0], 'digest': 'sha256:' + 'd' * 64})
        with self.assertRaisesRegex(ValueError, 'Conflicting'):
            publish.image_pins(manifest, 'v1')


class ContainerUpdateTests(unittest.TestCase):
    def test_failed_update_restores_containers_without_replacing_live_data(self):
        spec = importlib.util.spec_from_file_location('deploy_images', ROOT / 'deploy/deploy-images.py')
        deploy = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(deploy)
        with tempfile.TemporaryDirectory() as temp:
            base = Path(temp)
            (base / "backups").mkdir()
            old = {'.env': 'JWT_KEY=existing-test-key\nAPI_IMAGE=previous\n', 'compose.yaml': 'previous compose',
                   'active-release.json': '{"version":"previous"}', 'gateway.conf': 'previous gateway'}
            for name, content in old.items():
                (base / name).write_text(content)
            (base / 'data').mkdir()
            (base / 'data/app.db').write_bytes(b'live data with new content')
            release = base / 'image-releases/v1'
            release.mkdir(parents=True)
            recipe = {'version': 'v1', 'source': {}, 'images': {key: publish.REGISTRY + '@sha256:' + digit * 64
                      for key, digit in [('API_IMAGE','a'),('WEB_IMAGE','b'),('GATEWAY_IMAGE','c')]}}
            (release / 'release.json').write_text(json.dumps(recipe))
            (release / 'compose.images.yaml').write_text('new compose')
            (release / 'gateway.conf.template').write_text('__SWAG_IP__ new gateway')
            calls = []
            def compose(*args):
                calls.append(args)
                if args[0] == 'up' and len([call for call in calls if call[0] == 'up']) == 1:
                    raise RuntimeError('new container failed')
            original_umask = deploy.os.umask(0o077)
            try:
                with patch.object(deploy, 'BASE', base), patch.object(deploy.sys, 'argv', ['deploy','v1']), \
                     patch.object(deploy, 'featured_state', return_value={'categories':[1], 'photos':['p']}), \
                     patch.object(deploy, 'run') as run, patch.object(deploy, 'compose', side_effect=compose), \
                     patch.object(deploy.subprocess, 'check_output', return_value='[{"NetworkSettings":{"Networks":{"swag":{"IPAddress":"172.18.0.2"}}}}]'):
                    with self.assertRaisesRegex(RuntimeError, 'new container failed'):
                        deploy.main()
                    commands = [call.args[0] for call in run.call_args_list]
                    self.assertTrue(all(command[:2] == ['docker','pull'] for command in commands[:3]))
                    self.assertEqual(commands[3], ['docker','stop','starblog-api-v2'])
                    self.assertEqual(commands[4][:3], ['docker','cp','starblog-api-v2:/app/data'])
                for name, content in old.items():
                    self.assertEqual((base / name).read_text(), content)
                self.assertEqual((base / 'data/app.db').read_bytes(), b'live data with new content')
                self.assertEqual(len([call for call in calls if call[0] == 'up']), 2)
            finally:
                deploy.os.umask(original_umask)


class SeoVerificationTests(unittest.TestCase):
    def setUp(self):
        spec = importlib.util.spec_from_file_location('deploy_images', ROOT / 'deploy/deploy-images.py')
        self.deploy = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(self.deploy)
        self.home = '<title>技术、摄影与生活手记 · StarBlog</title><link rel="canonical" href="https://blog.deali.cn/">'
        self.responses = {
            '/blog': b'<link rel="canonical" href="https://blog.deali.cn/blog">',
            '/photography': b'<link rel="canonical" href="https://blog.deali.cn/photography">',
            '/robots.txt': b'Sitemap: https://blog.deali.cn/sitemap.xml',
            '/sitemap.xml': b'<sitemapindex><sitemap><loc>https://blog.deali.cn/sitemaps/pages/1.xml</loc></sitemap></sitemapindex>',
            '/sitemaps/pages/1.xml': b'<urlset><url><loc>https://blog.deali.cn/</loc></url></urlset>',
        }

    def verify(self, home=None):
        with patch.object(self.deploy, 'read_url', side_effect=self.responses.__getitem__):
            self.deploy.verify_seo(self.home if home is None else home)

    def test_web_metadata_and_sitemap_chunks_pass(self):
        self.verify()

    def test_stale_api_sitemap_routing_is_rejected(self):
        self.responses['/sitemap.xml'] = b'<urlset><url><loc>https://blog.deali.cn/</loc></url></urlset>'
        with self.assertRaisesRegex(ValueError, 'Web sitemap index'):
            self.verify()

    def test_homepage_canonical_inherited_by_a_section_is_rejected(self):
        self.responses['/photography'] = b'<link rel="canonical" href="https://blog.deali.cn/">'
        with self.assertRaisesRegex(ValueError, 'canonical mismatch: /photography'):
            self.verify()

    def test_generic_home_title_is_rejected(self):
        with self.assertRaisesRegex(ValueError, 'site identity'):
            self.verify(self.home.replace('技术、摄影与生活手记 · StarBlog', '首页'))


if __name__ == '__main__':
    unittest.main()

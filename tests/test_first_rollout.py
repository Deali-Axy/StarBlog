import importlib.util
import io
import json
from pathlib import Path
import tarfile
import tempfile
import unittest
from unittest.mock import patch


spec = importlib.util.spec_from_file_location(
    "first_rollout", Path(__file__).resolve().parents[1] / "deploy/install-release.py",
)
rollout = importlib.util.module_from_spec(spec)
spec.loader.exec_module(rollout)


class ReleaseArchiveTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.archive = self.root / "release.tar.gz"
        self.destination = self.root / "unpacked"
        self.destination.mkdir()

    def test_archive_cannot_write_outside_release(self):
        with tarfile.open(self.archive, "w:gz") as output:
            member = tarfile.TarInfo("../outside")
            member.size = 4
            output.addfile(member, io.BytesIO(b"data"))
        with self.assertRaisesRegex(ValueError, "Unsafe archive path"):
            rollout.extract_release(self.archive, self.destination)
        self.assertFalse((self.root / "outside").exists())

    def test_symlink_cannot_escape_release(self):
        with tarfile.open(self.archive, "w:gz") as output:
            member = tarfile.TarInfo("web/link")
            member.type = tarfile.SYMTYPE
            member.linkname = "../../outside"
            output.addfile(member)
        with self.assertRaisesRegex(ValueError, "Unsafe symlink"):
            rollout.extract_release(self.archive, self.destination)

    def test_traced_node_symlinks_inside_release_are_preserved(self):
        with tarfile.open(self.archive, "w:gz") as output:
            member = tarfile.TarInfo("web/node_modules/.pnpm/package/index.js")
            member.size = 4
            output.addfile(member, io.BytesIO(b"code"))
            member = tarfile.TarInfo("web/node_modules/package")
            member.type = tarfile.SYMTYPE
            member.linkname = ".pnpm/package"
            output.addfile(member)
        rollout.extract_release(self.archive, self.destination)
        self.assertEqual((self.destination / "web/node_modules/package/index.js").read_bytes(), b"code")

    def test_private_input_archive_rejects_unexpected_members(self):
        migration = self.root / "migration"
        migration.mkdir()
        with tarfile.open(migration / "inputs.tar.gz", "w:gz") as output:
            member = tarfile.TarInfo("../identity.json")
            data = json.dumps([["id", "admin", "hash"]]).encode()
            member.size = len(data)
            output.addfile(member, io.BytesIO(data))
        with patch.object(rollout, "BASE", self.root):
            with self.assertRaisesRegex(ValueError, "Unexpected private migration inputs"):
                rollout.read_inputs()
        self.assertFalse((self.root / "identity.json").exists())


if __name__ == "__main__":
    unittest.main()

let toastTrigger = document.getElementById('liveToastBtn')
let toastLiveExample = document.getElementById('liveToast')
if (toastTrigger) {
    toastTrigger.addEventListener('click', function () {
        let toast = new bootstrap.Toast(toastLiveExample)

        toast.show()
    })
}

class TocNode {
    constructor(text, href, tags, nodes) {
        this.text = text
        this.href = href
        this.tags = tags
        this.nodes = nodes
    }
}

$(function () {
    let editorMdView = editormd.markdownToHTML("test-editormd-view", {
        // htmlDecode: "style,script,iframe",  // you can filter tags decode
        htmlDecode: true,
        //toc             : false,
        tocm: true,    // Using [TOCM]
        tocContainer: "#custom-toc-container", // 自定义 ToC 容器层
        //gfm             : false,
        //tocDropdown     : true,
        // markdownSourceCode : true, // 是否保留 Markdown 源码，即是否删除保存源码的 Textarea 标签
        emoji: true,
        taskList: true,
        tex: true,  // 默认不解析
        flowChart: true,  // 默认不解析
        sequenceDiagram: true,  // 默认不解析
    });

    let toc = editorMdView.markdownToC

    console.log(toc)

    // todo 尝试转换为树，结果失败。。。
    function getNodes(index = 0) {
        let nodes = []

        for (let i = index; i < toc.length - 1; i++) {
            let item = toc[i]
            let nextItem = toc[i + 1]
            let node = new TocNode(item.text, `#${item.text}`, null, null)

            if (item.level === nextItem.level) {
                nodes.push(node)
            }
            if (item.level < nextItem.level) {
                node.nodes = getNodes(i + 1)
                nodes.push(node)
                i += node.nodes.length
                continue
            }
            if (item.level > nextItem.level) {
                nodes.push(node)
                i++
                break
            }
        }

        return nodes
    }

    let nodeList = getNodes()
    console.log(nodeList)

})

/**
 * 转换文章里的图片链接
 *
 * @deprecated 现在不需要了，直接在后端处理
 * @param postId
 */
function procImages(postId) {
    $.get(`/Api/BlogPost/${postId}/`, function (res) {
        console.log(res)
        for (const imgElem of document.querySelectorAll('.post-content img')) {
            let originSrc = imgElem.getAttribute('src')
            // let newSrc = `/assets/blog/${res.data.path}/${originSrc}`
            let newSrc = `/media/blog/${res.data.id}/${originSrc}`
            console.log('originSrc', originSrc)
            console.log('newSrc', newSrc)
            imgElem.setAttribute('src', newSrc)
        }
    })
}
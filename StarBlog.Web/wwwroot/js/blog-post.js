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
    let editorMdView = editormd.markdownToHTML("post-markdown-content", {
        // htmlDecode: "style,script,iframe",  // you can filter tags decode
        htmlDecode: true,
        //toc             : false,
        tocm: true,    // Using [TOCM]
        // tocContainer: "#toc-container", // 自定义 ToC 容器层
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
    for (let i = 0; i < toc.length; i++) {
        let item = toc[i]
        item.id = i
        item.pid = -1
        for (let j = i; j >= 0; j--) {
            let preItem = toc[j]
            if (item.level === preItem.level + 1) {
                item.pid = j
                break
            }
        }
    }

    function getNodes(pid = -1) {
        let nodes = toc.filter(item => item.pid === pid)
        if (nodes.length === 0) return null

        return nodes.map(item => new TocNode(item.text, `#${item.text}`, null, getNodes(item.id)))
    }

    let nodes = getNodes()

    $('#post-toc-container').treeview({
        data: nodes,
        levels: 2,
        enableLinks: true,
        highlightSelected: false,
        showTags: true,
        enableIndent: false,
        onNodeSelected: function (event, data) {
            console.log(data)
        },
        onNodeUnselected: function (event, data) {
        },
        // selectedBackColor: "rgba(220, 2, 7, 0.68)",
        // onhoverColor: "rgba(0,0,0,.8)",
        // showBorder: false,
    })
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
const initTreeView = function (data) {
    return $('#post-toc-container').treeview({
        data,
        levels: 2,
        enableLinks: true,
        highlightSelected: false,
        showTags: true,
    })
}

$(function () {
    let markdownContent = document.getElementById('post-markdown-content')
    if (markdownContent) {
        let editorMdView = editormd.markdownToHTML("post-markdown-content", {
            htmlDecode: true,
            tocm: true,    // Using [TOCM]
            emoji: true,
            taskList: true,
            tex: true,  // 默认不解析
            flowChart: true,  // 默认不解析
            sequenceDiagram: true,  // 默认不解析
        });

        initTreeView(editorMdView.markdownTocTree)
    }
})

/**
 * 转换文章里的图片链接
 *
 * @deprecated 现在不需要了，直接在后端处理
 * @param postId
 */
function procImages(postId) {
    $.get(`/Api/BlogPost/${postId}/`, function (res) {
        console.log('procImages', res)
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
$(function () {
    let testEditormdView = editormd.markdownToHTML("test-editormd-view", {
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
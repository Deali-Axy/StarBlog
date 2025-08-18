const initTreeView = function (data) {
    return $('#categories').treeview({
        data: data,
        levels: 1,
        enableLinks: true,
        highlightSelected: false,
        showTags: true,
        onNodeSelected: function (event, data) {
            console.log(data)
        },
        onNodeUnselected: function (event, data) {
        },
        // selectedBackColor: "rgba(220, 2, 7, 0.68)",
        // onhoverColor: "rgba(0,0,0,.8)",
        // showBorder: false,
    })
}
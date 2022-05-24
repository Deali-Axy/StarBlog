const initTreeView = function (data) {
    return $('#categories').treeview({
        data: data,
        levels: 1,
        collapseIcon: "fa fa-caret-down",
        enableLinks: true,
        expandIcon: "fa fa-caret-right",
        emptyIcon: 'fa fa-circle-o',
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
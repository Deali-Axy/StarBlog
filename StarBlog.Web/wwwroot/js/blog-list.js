const collections = [
    {
        text: 'Parent 1',
        href: '#parent1',
        tags: ['4'],
        nodes: [
            {
                text: 'Child 1',
                href: '#child1',
                tags: ['2'],
                nodes: [
                    {
                        text: 'Grandchild 1',
                        href: '#grandchild1',
                        tags: ['0']
                    },
                    {
                        text: 'Grandchild 2',
                        href: '#grandchild2',
                        tags: ['0']
                    }
                ]
            },
            {
                text: 'Child 2',
                href: '#child2',
                tags: ['0']
            }
        ]
    },
    {
        text: 'Parent 2',
        href: '#parent2',
        tags: ['0']
    },
    {
        text: 'Parent 3',
        href: '#parent3',
        tags: ['0']
    },
    {
        text: 'Parent 4',
        href: '#parent4',
        tags: ['0']
    },
    {
        text: 'Parent 5',
        href: '#parent5',
        tags: ['0']
    }
];

const instance = $('#categories').treeview({
    data: collections,
    collapseIcon: "fa fa-caret-down",
    expandIcon: "fa fa-caret-right",
    emptyIcon: 'fa fa-circle-o'
    // onNodeSelected: function (event, data) {
    // },
    // onNodeUnselected: function (event, data) {
    // },
    // selectedBackColor: "rgba(220, 2, 7, 0.68)",
    // onhoverColor: "rgba(0,0,0,.8)",
    // showBorder: false,
});
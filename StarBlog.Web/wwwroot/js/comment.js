new Vue({
    el: '#vue-comment',
    data: {
        comments: [],
        total: 0,
        page: 1,
        pageSize: 5,
        search: '',
        sortBy: ''
    },
    created: function () {
        this.getComments()
    },
    methods: {
        getComments() {
            let params = {
                PostId: POST_ID,
                Page: this.page,
                PageSize: this.pageSize,
            }
            if (this.search) params.Search = this.search
            if (this.sortBy) params.SortBy = this.sortBy

            fetch(`/Api/Comment?` + new URLSearchParams(params))
                .then(res => res.json()).then(res => {
                console.log(res)
                this.comments = res.data
                this.total = res.pagination.totalItemCount
            })
        }
    }
})
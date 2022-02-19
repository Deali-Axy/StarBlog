let homeApp = new Vue({
    el: '#vue-app',
    data: {
        poem: {},
        hitokoto: {},
        poemSimple: ''
    },
    created: function () {
        fetch('http://dc.sblt.deali.cn:9800/poem/simple')
            .then(res => res.text()).then(data => this.poemSimple = data)
        fetch('http://dc.sblt.deali.cn:9800/poem/tang')
            .then(res => res.json())
            .then(data => {
                this.poem = data.data
            })
        fetch('http://dc.sblt.deali.cn:9800/hitokoto/get')
            .then(res => res.json())
            .then(data => {
                this.hitokoto = data.data[0]
            })
    }
})
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
    },
    methods: {
        /**
         * 声明一个可以创建随机颜色的函数，用来给词云加颜色
         * @returns {string}
         */
        randomColor() {
            return 'rgb(' + [
                Math.round(Math.random() * 255),
                Math.round(Math.random() * 255),
                Math.round(Math.random() * 255)
            ].join(',') + ')'
        },
        loadWordCloud() {
            fetch('/Api/Category/WordCloud').then(res => res.json())
                .then(res => {
                    const data = res.map(val => ({
                        ...val,
                        textStyle: {
                            normal: {
                                color: this.randomColor()
                            }
                        }
                    }))
                    console.log('data', data)
                })
        }
    }
})



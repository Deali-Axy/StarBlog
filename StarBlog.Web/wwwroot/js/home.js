const HEIGHT = 400

let homeApp = new Vue({
    el: '#vue-app',
    data: {
        poem: {},
        hitokoto: {},
        poemSimple: '',
        chartTypes: ['bubble', 'bar'],
        currentChartTypeIndex: 0,
        currentChart: null
    },
    computed: {
        chartElem() {
            return document.getElementById('myChart')
        }
    },
    created() {
        this.loadPoem()
        this.loadHitokoto()
    },
    mounted() {
        if (CHART_VISIBLE === true) this.loadChart()
    },
    methods: {
        loadPoem() {
            fetch('/Api/DataAcq/Poem')
                .then(res => res.json()).then(res => this.poemSimple = res.data)
        },
        loadHitokoto() {
            fetch('/Api/DataAcq/Hitokoto')
                .then(res => res.json()).then(res => this.hitokoto = res.data)
        },
        /**
         * 生成随机RGB颜色字符串，例如：rgb(123,123,123)
         * @returns {string}
         */
        randomRGB() {
            return 'rgb(' + this.randomColorArray().join(',') + ')'
        },
        // 生成随机RGBA字符串，例如：rgba(123,123,123,0.2)
        randomRGBA(a) {
            return this.convertRGBA(this.randomColorArray(), a)
        },
        // RGB数组转换成RGBA字符串
        convertRGBA(rgbArray, a) {
            let color = Array.from(rgbArray)
            color.push(a)
            return 'rgba(' + color.join(',') + ')'
        },
        randomColorArray() {
            return [
                Math.round(Math.random() * 255),
                Math.round(Math.random() * 255),
                Math.round(Math.random() * 255),
            ]
        },
        switchChartType() {
            if (this.currentChartTypeIndex >= this.chartTypes.length - 1)
                this.currentChartTypeIndex = 0
            else
                this.currentChartTypeIndex++
            if (this.currentChart)
                this.currentChart.destroy()
            this.chartElem.setAttribute('style', '')
            this.loadChart()
        },
        loadChart() {
            let chartType = this.chartTypes[this.currentChartTypeIndex]
            switch (chartType) {
                case 'bubble':
                    this.loadBubbleChart()
                    break
                case 'bar':
                    this.loadBarChart()
                    break
                default:
            }
        },
        loadBubbleChart() {
            fetch('/Api/Category/WordCloud').then(res => res.json())
                .then(res => {
                    let datasets = []
                    res.data.forEach(item => {
                        let color = this.randomColorArray()
                        datasets.push({
                            label: item.name,
                            data: [{
                                x: Math.round(Math.random() * 50),
                                y: Math.round(Math.random() * 50),
                                r: item.value
                            }],
                            backgroundColor: this.convertRGBA(color, 0.2),
                            borderColor: this.convertRGBA(color, 1),
                            borderWidth: 1
                        })
                    })

                    let data = {
                        datasets: datasets
                    };
                    let config = {
                        type: 'bubble',
                        data: data,
                        options: {
                            maintainAspectRatio: false,
                        }
                    };

                    this.currentChart = new Chart(this.chartElem, config)
                    this.currentChart.resize(null, HEIGHT)
                })
        },
        loadBarChart() {
            fetch('/Api/Category/WordCloud').then(res => res.json())
                .then(res => {
                    let labels = []
                    let values = []
                    let backgroundColors = []
                    let borderColors = []
                    res.data.forEach(item => {
                        labels.push(item.name)
                        values.push(item.value)
                        let color = this.randomColorArray()
                        backgroundColors.push(this.convertRGBA(color, 0.2))
                        borderColors.push(this.convertRGBA(color, 1))
                    })
                    let data = {
                        labels: labels,
                        datasets: [{
                            label: '# of Votes',
                            data: values,
                            backgroundColor: backgroundColors,
                            borderColor: borderColors,
                            borderWidth: 1
                        }]
                    }
                    let config = {
                        type: 'bar',
                        data: data,
                        options: {
                            maintainAspectRatio: false,
                        }
                    }

                    this.currentChart = new Chart(this.chartElem, config)
                    this.currentChart.resize(null, HEIGHT)
                })


        }
    }
})


// Enable tooltips
console.log('Enable tooltips')
const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]')
const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))
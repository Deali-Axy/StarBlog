let homeApp = new Vue({
    el: '#vue-app',
    data: {
        poem: {},
        hitokoto: {},
        poemSimple: ''
    },
    created() {
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
    mounted() {
        this.loadChart()
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
        },
        loadChart() {
            const data = {
                datasets: [{
                    label: 'First Dataset',
                    data: [{
                        x: 20,
                        y: 30,
                        r: 15
                    }, {
                        x: 40,
                        y: 10,
                        r: 10
                    }],
                    backgroundColor: 'rgb(255, 99, 132)'
                }]
            };
            const config = {
                type: 'bubble',
                data: data,
                options: {}
            };

            const labels = [
                'January',
                'February',
                'March',
                'April',
                'May',
                'June',
            ];
            const data2 = {
                labels: labels,
                datasets: [{
                    label: 'My First dataset',
                    backgroundColor: 'rgb(255, 99, 132)',
                    borderColor: 'rgb(255, 99, 132)',
                    data: [0, 10, 5, 2, 20, 30, 45],
                }]
            };

            const config2 = {
                type: 'line',
                data: data2,
                options: {}
            };

            const config3 = {
                type: 'bar',
                data: {
                    labels: ['Red', 'Blue', 'Yellow', 'Green', 'Purple', 'Orange'],
                    datasets: [{
                        label: '# of Votes',
                        data: [12, 19, 3, 5, 2, 3],
                        backgroundColor: [
                            'rgba(255, 99, 132, 0.2)',
                            'rgba(54, 162, 235, 0.2)',
                            'rgba(255, 206, 86, 0.2)',
                            'rgba(75, 192, 192, 0.2)',
                            'rgba(153, 102, 255, 0.2)',
                            'rgba(255, 159, 64, 0.2)'
                        ],
                        borderColor: [
                            'rgba(255, 99, 132, 1)',
                            'rgba(54, 162, 235, 1)',
                            'rgba(255, 206, 86, 1)',
                            'rgba(75, 192, 192, 1)',
                            'rgba(153, 102, 255, 1)',
                            'rgba(255, 159, 64, 1)'
                        ],
                        borderWidth: 1
                    }]
                },
                options: {
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            };

            let myChart = new Chart(document.getElementById('myChart'), config3)
        }
    }
})



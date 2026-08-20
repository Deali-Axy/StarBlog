new Vue({
    el: '#vue-comment',
    data: {
        comments: [],
        total: 0,
        page: 1,
        pageSize: 5,
        search: '',
        sortBy: '',
        refreshLoading: true,
        submitLoading: false,
        emailDisabled: false,
        emailOtpDisabled: false,
        getEmailOtpDisabled: false,
        getEmailOtpLoading: false,
        userNameDisabled: true,
        userNameLoading: false,
        urlDisabled: true,
        urlLoading: false,
        contentDisabled: true,
        submitDisabled: true,
        replyDisabled: true,
        replyComment: null,
        getEmailOtpText: '获取',
        otpInterval: null,
        form: {
            parentId: '',
            postId: POST_ID,
            userName: '',
            email: '',
            emailOtp: '',
            url: '',
            content: '',
        },
        formRules: {
            userName: [
                {required: true, message: '请输入用户名称', trigger: 'blur'},
                {min: 2, max: 20, message: '长度在 2 到 20 个字符', trigger: 'blur'}
            ],
            email: [
                {required: true, message: '请输入邮箱', trigger: 'blur'},
                {type: 'email', message: '邮箱格式不正确'}
            ],
            emailOtp: [
                {required: true, message: '请输入邮箱验证码', trigger: 'change'},
                {len: 4, message: '长度 4 个字符', trigger: 'change'}
            ],
            url: [
                {type: 'url', message: `请输入正确的url`, trigger: 'blur'},
            ],
            content: [
                {required: true, message: '请输入评论内容', trigger: 'blur'},
                {min: 1, max: 300, message: '长度 在 1 到 300 个字符', trigger: 'blur'},
                // {whitespace: true, message: '评论内容只存在空格', trigger: 'blur'},
            ]
        },
    },
    created: async function () {
        await this.getComments()
        this.refreshLoading = false
    },
    methods: {
        getEmailOtp(email) {
            return new Promise((resolve, reject) => {
                axios.get(`/Api/Comment/GetEmailOtp?email=${email}`)
                    .then(res => resolve(res.data))
                    .catch(res => resolve(res.response.data))
            })
        },
        getAnonymousUser(email, otp) {
            return new Promise((resolve, reject) => {
                axios.get(`/Api/Comment/GetAnonymousUser?email=${email}&otp=${otp}`)
                    .then(res => resolve(res.data))
                    .catch(res => resolve(res.response.data))
            })
        },
        getComments() {
            let params = {
                PostId: POST_ID,
                Page: this.page,
                PageSize: this.pageSize,
            }
            if (this.search) params.Search = this.search
            if (this.sortBy) params.SortBy = this.sortBy

            return new Promise((resolve, reject) => {
                axios.get(`/Api/Comment`, {params})
                    .then(res => {
                        this.comments = res.data.data.map(e => {
                            let item = {
                                ...e,
                                anonymousUser: {
                                    ...e.anonymousUser,
                                    createdTime: dayjs(e.anonymousUser.createdTime).format('YYYY-MM-DD'),
                                    updatedTime: dayjs(e.anonymousUser.updatedTime).format('YYYY-MM-DD'),
                                },
                                createdTime: dayjs(e.createdTime).format('YYYY-MM-DD'),
                                updatedTime: dayjs(e.updatedTime).format('YYYY-MM-DD'),
                                avatar: `/Api/PicLib/Random/100/100?Seed=${e.anonymousUserId}`,
                            }

                            if (e.parent) {
                                item.replyUser = e.parent.anonymousUser.name
                            }

                            return item
                        })
                        this.total = res.data.pagination.totalItemCount
                        resolve(res)
                    })
                    .catch(res => {
                        console.log(res)
                        let data = res.response.data
                        resolve(data)
                    })
            })
        },
        submitComment(data) {
            return new Promise((resolve, reject) => {
                axios.post(`/Api/Comment`, {...data})
                    .then(res => resolve(res.data))
                    .catch(res => resolve(res.response.data))
            })
        },
        async handleRefresh() {
            this.refreshLoading = true
            await this.getComments()
            this.refreshLoading = false
        },
        async handleGetEmailOtp() {
            if (!this.form.email) {
                this.$message.error('请输入邮箱地址！')
                return
            }
            this.getEmailOtpLoading = true
            let res = await this.getEmailOtp(this.form.email)
            this.getEmailOtpLoading = false
            if (res.successful) {
                this.$message.success(res.message)
                // 发送成功，显示倒计时
                let countdown = 60 * 5
                this.getEmailOtpText = `${countdown}s`
                this.otpInterval = setInterval(() => {
                    countdown--
                    if (countdown > 0) {
                        this.getEmailOtpText = `${countdown}s`
                        this.getEmailOtpDisabled = true
                    } else {
                        countdown = 60 * 5
                        this.getEmailOtpText = '获取'
                        if (this.otpInterval) clearInterval(this.otpInterval)
                        this.getEmailOtpDisabled = false
                    }
                }, 1000)
            } else {
                this.$message.error(res.message)
            }
        },
        async handleEmailOtpChange(value) {
            console.log('handleEmailOtpChange', value)
            if (this.form.email?.length === 0 || value.length < 4) return

            this.userNameLoading = true
            this.urlLoading = true
            let res = await this.getAnonymousUser(this.form.email, value)
            console.log(res)
            if (res.successful) {
                if (res.data.anonymousUser) {
                    this.form.userName = res.data.anonymousUser.name
                    this.form.url = res.data.anonymousUser.url
                }
                this.form.emailOtp = res.data.newOtp
                // 锁住邮箱和验证码，不用编辑了
                this.getEmailOtpDisabled = true
                this.emailDisabled = true
                this.emailOtpDisabled = true
                // 开启编辑用户名、网址、内容、回复
                this.userNameDisabled = false
                this.urlDisabled = false
                this.contentDisabled = false
                this.submitDisabled = false
                this.replyDisabled = false
            } else {
                this.$message.error(res.message)
            }
            this.userNameLoading = false
            this.urlLoading = false
        },
        handleUrlBlur() {
            if (!this.form.url.startsWith('http'))
                this.form.url = `http://${this.form.url}`
        },
        handleReset() {
            this.$refs.form.resetFields()
            if (this.otpInterval) clearInterval(this.otpInterval)
            this.getEmailOtpText = '获取'
            this.emailDisabled = false
            this.emailOtpDisabled = false
            this.getEmailOtpDisabled = false
            this.userNameDisabled = true
            this.urlDisabled = true
            this.contentDisabled = true
            this.submitDisabled = true
            this.replyDisabled = true
            this.form.parentId = ''
            this.replyComment = null
        },
        async handleSubmit() {
            this.$refs.form.validate(async (valid) => {
                if (valid) {
                    this.submitLoading = true
                    let res = await this.submitComment(this.form)
                    if (res.successful) {
                        this.$message.success(res.message)
                        let email = `${this.form.email}`
                        this.handleReset()
                        this.form.email = email
                    } else this.$message.error(res.message)
                    this.submitLoading = false
                    await this.getComments()
                }
            })
        },
        async handleSizeChange(value) {
            this.pageSize = value
            await this.handleRefresh()
        },
        async handleCurrentChange(value) {
            this.page = value
            await this.handleRefresh()
        },
        async handleReply(comment) {
            this.replyComment = comment
            this.form.parentId = comment.id
            this.$refs.content.focus()
        },
        handleReplyTagClose() {
            this.form.parentId = ''
            this.replyComment = null
        }
    }
})
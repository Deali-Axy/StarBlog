/// <binding BeforeBuild='min' Clean='clean' ProjectOpened='auto' />
"use strict";

//加载使用到的 gulp 插件
const gulp = require("gulp"),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-clean-css"),
    rename = require("gulp-rename"),
    uglify = require("gulp-uglify"),
    changed = require("gulp-changed");


//定义 wwwroot 下的各文件存放路径
const paths = {
    root: "./wwwroot/",
    css: './wwwroot/css/',
    js: './wwwroot/js/',
    lib: './wwwroot/lib/'
};

//css
paths.cssDist = paths.css + "**/*.css";//匹配所有 css 的文件所在路径
paths.minCssDist = paths.css + "**/*.min.css";//匹配所有 css 对应压缩后的文件所在路径
paths.concatCssDist = paths.css + "app.min.css";//将所有的 css 压缩到一个 css 文件后的路径

//js
paths.jsDist = paths.js + "**/*.js";//匹配所有 js 的文件所在路径
paths.minJsDist = paths.js + "**/*.min.js";//匹配所有 js 对应压缩后的文件所在路径
paths.concatJsDist = paths.js + "app.min.js";//将所有的 js 压缩到一个 js 文件后的路径


//使用 npm 下载的前端组件包
const libs = [
    {name: "jquery", dist: "./node_modules/jquery/dist/**/*.*"},
    {name: "popper", dist: "./node_modules/popper.js/dist/**/*.*"},
    {name: "bootstrap", dist: "./node_modules/bootstrap/dist/**/*.*"},
    {name:"bootswatch",dist: "./node_modules/bootswatch/dist/**/*.*"}
];

//清除压缩后的文件
gulp.task("clean:css", done => rimraf(paths.minCssDist, done));
gulp.task("clean:js", done => rimraf(paths.minJsDist, done));

gulp.task("clean", gulp.series(["clean:js", "clean:css"]));

//移动 npm 下载的前端组件包到 wwwroot 路径下
gulp.task("move", done => {
    libs.forEach(function (item) {
        gulp.src(item.dist)
            .pipe(gulp.dest(paths.lib + item.name + "/dist"));
    });
    done()
});

//每一个 css 文件压缩到对应的 min.css
gulp.task("min:css", () => {
    return gulp.src([paths.cssDist, "!" + paths.minCssDist], {base: "."})
        .pipe(rename({suffix: '.min'}))
        .pipe(changed('.'))
        .pipe(cssmin())
        .pipe(gulp.dest('.'));
});

//将所有的 css 文件合并打包压缩到 app.min.css 中
gulp.task("concatmin:css", () => {
    return gulp.src([paths.cssDist, "!" + paths.minCssDist], {base: "."})
        .pipe(concat(paths.concatCssDist))
        .pipe(changed('.'))
        .pipe(cssmin())
        .pipe(gulp.dest("."));
});

//每一个 js 文件压缩到对应的 min.js
gulp.task("min:js", () => {
    return gulp.src([paths.jsDist, "!" + paths.minJsDist], {base: "."})
        .pipe(rename({suffix: '.min'}))
        .pipe(changed('.'))
        .pipe(uglify())
        .pipe(gulp.dest('.'));
});

//将所有的 js 文件合并打包压缩到 app.min.js 中
gulp.task("concatmin:js", () => {
    return gulp.src([paths.jsDist, "!" + paths.minJsDist], {base: "."})
        .pipe(concat(paths.concatJsDist))
        .pipe(changed('.'))
        .pipe(uglify())
        .pipe(gulp.dest("."));
});

gulp.task("min", gulp.series(["min:js", "min:css"]));
gulp.task("concatmin", gulp.series(["concatmin:js", "concatmin:css"]));


//监听文件变化后自动执行
gulp.task("auto", () => {
    gulp.watch(paths.css, gulp.series(["min:css", "concatmin:css"]));
    gulp.watch(paths.js, gulp.series(["min:js", "concatmin:js"]));
});
/**
 * 动态加载CSS
 * 参考：http://lengyun.github.io/js/3-2-2dynamicAddCSS.html
 */
function loadStyles(url) {
    let link = document.createElement("link");
    link.rel = "stylesheet";
    link.type = "text/css";
    link.href = url;
    let head = document.getElementsByTagName("head")[0];
    head.appendChild(link);
}

let currentTheme = localStorage.getItem('currentTheme')
if (currentTheme !== 'Bootstrap') {
    let themeCssUrl = localStorage.getItem('currentThemeCssUrl')
    if (themeCssUrl != null) loadStyles(themeCssUrl)
}

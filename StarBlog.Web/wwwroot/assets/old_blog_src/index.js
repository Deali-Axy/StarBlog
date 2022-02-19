window.addEventListener("error", function (e) {
    var stack = e.error.stack;
    var message = e.error.toString();

    if (stack) {
        message += "\n" + stack;
    }

    console.log(stack);

    let xhr = new XMLHttpRequest();
    xhr.open("POST", "/log", true);
    // Fire an Ajax request with error details
    xhr.send(message);
});

let animationSpeed = 1000;

let poem;
let poemIndex = 0;
fetch('http://dc.sblt.deali.cn:9800/poem/tang').then(res => res.json())
    .then(result => {
        poem = result.data.content.join('');
        console.log(poem);
        for (let tile of document.getElementsByClassName('tile-sm')) {
            let divContent = document.createElement('div');
            divContent.className = 'tile-content';
            if (poemIndex >= poem.length - 1)
                poemIndex = 0;
            divContent.innerHTML = poem[poemIndex++];
            divContent.hidden = true;

            let divBg = document.createElement('div');
            divBg.className = 'tile-bg';
            divBg.style.background = randomImageUrl(100, 100);
            divBg.style.backgroundColor = randomColor();

            tile.appendChild(divBg);
            tile.appendChild(divContent);
            setFlipInterval(tile);
        }
    })

for (let tile of document.querySelectorAll('.tile-md')) {
    let divBg = document.createElement('div');
    divBg.className = 'tile-bg';
    divBg.style.background = randomImageUrl(205, 205);
    divBg.style.backgroundColor = randomColor();
    tile.appendChild(divBg);
    setFlipInterval(tile);
}

for (let tile of document.querySelectorAll('.tile-wd')) {
    let divBg = document.createElement('div');
    divBg.className = 'tile-bg';
    divBg.style.background = randomImageUrl(415, 205);
    divBg.style.backgroundColor = randomColor();
    tile.appendChild(divBg);
    setFlipInterval(tile);
}

for (let tile of document.querySelectorAll('.tile-lg')) {
    let divBg = document.createElement('div');
    divBg.className = 'tile-bg';
    divBg.style.background = randomImageUrl(415, 415);
    divBg.style.backgroundColor = randomColor();
    tile.appendChild(divBg);
    setFlipInterval(tile);
}

function randomImageUrl(width, height) {
    let url = `https://picsum.photos/${width}/${height}?random=${Math.round(Math.random() * 100)}`;
    // console.log(url);
    return `url("${url}")`;
}

function randomColor() {
    return `rgba(${Math.round(Math.random() * 255)},${Math.round(Math.random() * 255)},${Math.round(Math.random() * 255)},1)`;
}

//生成从minNum到maxNum的随机数
function randomNum(minNum, maxNum) {
    switch (arguments.length) {
        case 1:
            return parseInt(Math.random() * minNum + 1, 10);
        case 2:
            return parseInt(Math.random() * (maxNum - minNum + 1) + minNum, 10);
        default:
            return 0;
    }
}

function setFlipInterval(tile) {
    let tileContent = tile.querySelector('.tile-content');

    return setInterval(() => {
        setTimeout(() => {
            // 这里做判断分两种情况，看看是转到正面还是反面
            // 正面
            if (tile.getAttribute('init') == null) {
                // 旋转动画
                tile.style.animation = `tile-flip ${animationSpeed / 500}s linear infinite`;
                // 背景变暗动画
                tile.querySelector('.tile-bg').style.animation = `tile-bg-dark ${animationSpeed / 500}s linear infinite`;
                // 内容逐渐出现动画
                if (tileContent) {
                    tileContent.hidden = false;
                    tileContent.style.animation = `tile-content-show ${animationSpeed / 250}s linear infinite`;
                }
                tile.setAttribute('init', 'false');
                setTimeout(() => {
                    // 背景配合动画进行反转
                    tile.style.transform = `rotateY(180deg)`;
                    // 关闭磁贴翻转动画
                    tile.style.animation = 'none';
                    // 关闭背景渐变动画
                    tile.querySelector('.tile-bg').style.animation = 'none';
                    // 关闭内容动画
                    if (tileContent) tileContent.style.animation = 'none';
                    // 配合渐变动画将背景变暗
                    tile.querySelector('.tile-bg').classList.add('tile-bg-dim');
                }, animationSpeed);
            }
            // 反面
            else {
                // 旋转动画
                tile.style.animation = `tile-flip2 ${animationSpeed / 500}s linear infinite`;
                // 背景变亮动画
                tile.querySelector('.tile-bg').style.animation = `tile-bg-light ${animationSpeed / 500}s linear infinite`;
                tile.removeAttribute('init');
                // 内容逐渐消失动画
                if (tileContent) {
                    tileContent.style.animation = `tile-content-hide ${animationSpeed / 250}s linear infinite`;
                }
                setTimeout(() => {
                    tile.style.transform = `rotateY(0)`;
                    // 关闭磁贴翻转动画
                    tile.style.animation = 'none';
                    // 关闭背景动画
                    tile.querySelector('.tile-bg').style.animation = 'none';
                    // 关闭内容动画
                    if (tileContent) {
                        tileContent.style.animation = 'none';
                        tileContent.hidden = true;
                    }
                    tile.querySelector('.tile-bg').className = 'tile-bg';
                }, animationSpeed);
            }
        }, animationSpeed);

    }, randomNum(8000, 15000));
}
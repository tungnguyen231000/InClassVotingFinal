window.onload = function () {
    const process = document.querySelectorAll('#data-process');
    const colorX = document.querySelectorAll('#data-color');
    const total = document.querySelector('#data-total');

    var arrColor = [];
    var arrProcess = [];


    function getRandomColor() {
        var letters = '0123456789ABCDEF';
        var color = '#';
        for (var i = 0; i < 6; i++) {
            color += letters[Math.floor(Math.random() * 16)];
        }
        return color;
    }

    for (var i = 0; i < process.length; i++) {
        arrColor.push(getRandomColor());
    }

    process.forEach((item) => {
        arrProcess.push(parseInt((parseInt(item.textContent) / parseInt(total.textContent)) * 100));
    });

    for (var i = 0; i < colorX.length; i++) {
        colorX[i].style.background = arrColor[i];
        /*
        if (arrProcess[i]==100) {
            process[i].style.background = arrColor[i];
        }
        */
    }

    for (var i = 0; i < colorX.length; i++) {
        colorX[i].style.width = arrProcess[i] + '%';
    }



};
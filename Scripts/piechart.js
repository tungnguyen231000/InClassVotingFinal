window.onload = function () {
    const stuff = document.querySelectorAll('#data-stuff');
    const string = document.querySelectorAll('#data-string');
    const colorX = document.querySelectorAll('#data-color');

    var arrColor = [];
    var colorIndex = [];

    var arrStuff = [];
    var arrString = [];
    var background = '';
    var persent = 0;


    var checkStuff = [];
    var checkString = [];
    var checkduplicate = [];

    for (let i = 0; i < arrColor.length; i++) {
        colorIndex.push(arrColor[i]);
    }

    //Đẩy array vào mảng mới
    stuff.forEach((item) => {
        arrStuff.push(item.textContent);
        checkStuff.push(item.textContent);
        checkduplicate.push(item.textContent);
    });


    string.forEach((item) => {
        arrString.push(item.textContent);
        checkString.push(item.textContent);
    });

    

    //Sort array
    arrStuff.sort(function (a, b) {
        return a - b;
    });

    

    for (let i = 0; i < arrStuff.length; i++) {
        let numberofduplicate = 0;
        //check số lượng số trùng lặp trong mảng
        for (let k = 0; k < checkduplicate.length; k++) {
            if (arrStuff[i] == checkduplicate[k]) {
                ++numberofduplicate;
            }
        }

        for (let j = 0; j < checkStuff.length; j++) {

            if (arrStuff[i] == checkStuff[j]) {
                if (numberofduplicate == 1) {
                    arrString[i] = checkString[j];
                    numberofduplicate = 0;

                    let index = checkduplicate.indexOf(arrStuff[i]);
                    if (index > -1) {
                        checkduplicate.splice(index, 1);
                    }
                } else {
                    
                    --numberofduplicate;
                }

            }
        }
    }
    for (let i = 0; i < arrString.length; i++) {
        string[i].innerHTML = arrString[i];
    }


    function getRandomColor() {
        var letters = '0123456789ABCDEF';
        var color = '#';
        for (var i = 0; i < 6; i++) {
            color += letters[Math.floor(Math.random() * 16)];
        }
        return color;
    }

    //Đẩy màu ngẫu nhiên vào bảng
    for (let i = 0; i < stuff.length; i++) {
        arrColor.push(getRandomColor());
    }
   

    //Thay đổi màu theo array đã sort
    for (let i = 0; i < colorX.length; i++) {
        colorX[i].style.color = arrColor[i];
    }

    background = 'radial-gradient(circle closest-side, transparent 180px, white 0), conic-gradient( ';

    for (var i = 0; i < colorX.length; i++) {

        persent += parseFloat(arrStuff[i]);

        stuff[i].innerHTML = parseFloat(arrStuff[i]) + "";

        if (persent < 99) {
            background += arrColor[i] + ' ' + '0%  , ' + arrColor[i] + ' ' + persent + '% , ';
        } else {
            background += arrColor[i] + ' ' + '0% , ' + arrColor[i] + ' ' + persent + '% ';
        }
    }
    background += ')';
    $(".pie-chart").css('background', background);


    $(document).ready(function(){
        let x = ($("#fig").height()*2 + 550) + "px";
        let y = (($("#fig").height() * 2 + 450)/2 + 300) + "px";

        $('.pie-chart').css('min-height', x );
        $("#fig").css("padding-top", y);
    });


    
};
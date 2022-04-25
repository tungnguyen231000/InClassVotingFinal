window.onload = function () {

    const numbs = document.querySelector("#number");
    const numb = document.querySelector(".numb");

    const persent = parseInt(numbs.getAttribute("data-persent")) + 1;


    let conturs = 440 - 440 * (persent / 100);
    let conter = 0;


    setInterval(() => {
        if (conturs == 0) {
            clearInterval();
        } else {
            const numbss = document.querySelector(".numb").style.strokeDashoffset = 'calc(' + conturs + ')';
            numb.textContent = numbss + "%";
        }
    }, 30);


    setInterval(() => {
        if (conter == (persent - 1)) {
            clearInterval();
        } else {
            conter += 1;
            numbs.textContent = conter + "%";
        }
    }, 30 / (((persent - 1) / 100)));


};
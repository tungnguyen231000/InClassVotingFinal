$(document).ready(function () {

	if ($(".drag-area img").length != 0) {
		const dragArea2 = document.querySelector(".drag-area-preview");
		const img = document.querySelector(".drag-area img");
		dragArea2.innerHTML = img.outerHTML;
    }

const dragArea = document.querySelector(".drag-area");
const header = document.querySelector(".header"),
	btnBrowser = document.querySelector(".btn-image"),
	input = document.querySelector(".file-input");

var defaulte = '<div><i class="icon fas fa-cloud-upload-alt"></i></div>' +
	'<div><h3 class="header">Drag and Drop to Upload File</h3></div>' +
	'<input name="imageExisted" value="0" type="hidden" />';

let file;
//Thêm files và checkImage -new
let files;
let checkImage = false;


$(btnBrowser).click(function () {
	$(input).trigger('click');
})

	input.addEventListener("change", function (e) {
		file = this.files[0];
		files = e.target.files;
		show(file);

	});


// di chuyển bên trong
dragArea.addEventListener("dragover", (event) => {
	event.preventDefault();
	dragArea.classList.add('active');
	header.textContent = 'Release to Upload File';
});

// di chuyển bên ngoài
dragArea.addEventListener("dragleave", () => {
	dragArea.classList.remove('active');
	header.textContent = 'Drag and Drop to Upload File';
});

// thả bên trong
dragArea.addEventListener("drop", (event) => {
	event.preventDefault();

	file = event.dataTransfer.files[0];


	//Thêm files -new
	files = event.dataTransfer.files;

	show(file);


	//Check ảnh -new
	if (checkImage == true) {
		let validImage = ['image/jpeg', 'image/jpg', 'image/png'];
		if (validImage.includes(file.type)) {
			input.files = event.dataTransfer.files;
		} else {
			$(".file-input").val(null);
			const dragArea2 = document.querySelector(".drag-area-preview");
			dragArea2.innerHTML = "";
			dragArea.innerHTML = defaulte;
        }
	}
	if (checkImage == false) {
		$(".file-input").val(null);
		const dragArea2 = document.querySelector(".drag-area-preview");
		dragArea2.innerHTML = "";
		dragArea.innerHTML = defaulte;
	}
});

function show(file) {
	let validImage = ['image/jpeg', 'image/jpg', 'image/png'];
	if (validImage.includes(file.type)) {
		let fileReader = new FileReader();
		fileReader.onload = () => {
			let fileURL = fileReader.result;
			let imgTag = '<img src="' + fileURL + '" alt="">' +
						 '<input name="imageExisted" value="1" type="hidden" />';
			dragArea.innerHTML = imgTag;


			//validate ảnh và vất vào preview - new
			if (file.size > 4 * 1024 * 1024) {
				if ($('.image-error').length == 0) {
					$(".file-input").after('<div class="image-error">*Image size exceeds 4MB</div>');
					$('.image-error').css("color", "red");
					$('.image-error').css("font-weight", "bold");
				}
				$(".file-input").val(null);
				dragArea.innerHTML = defaulte;
				checkImage = false;
			} else {
				$('.image-error').remove();
				input.files = files;
				checkImage = true;
			}

			if ($(".drag-area-preview").length != 0 && checkImage == true) {
				const dragArea2 = document.querySelector(".drag-area-preview");
				dragArea2.innerHTML = imgTag;
			} else {
				const dragArea2 = document.querySelector(".drag-area-preview");
				dragArea2.innerHTML = "";
			}



		}
		fileReader.readAsDataURL(file);
	} else {
		alert('This is not a Image File');
	}

	}

	if ($('.delete-img').length != 0) {
		$('.delete-img').click(function () {

			const dragArea2 = document.querySelector(".drag-area-preview");
			dragArea2.innerHTML = "";
			dragArea.innerHTML = defaulte;
			$(".file-input").val(null);
		});
	}
});
var courseId = "";
//get course id from the course user want to create/edit/delete new Chapter
function getCourseId(val) {
    courseId = val;
    document.getElementById("newChapCID").value = courseId;
}

var currentCouseID = "";
var currentCourseName = "";
//get course id from the course user want to edit Course Name
function getCourseIdToUpdate(cid, name) {
    currentCouseID = cid;
    currentCourseName = name;
    document.getElementById("cIdToUpdate").value = currentCouseID;
    document.getElementById("newCourseName").value = currentCourseName;
    document.getElementById("courseNameUpdate").innerHTML = currentCourseName;
}

function getCourseIdToDelete(cid, cname) {
    currentCouseID = cid;
    document.getElementById("cIdToDelete").value = currentCouseID;
    currentCourseName = cname;
    document.getElementById("courseNameDelete").innerHTML = currentCourseName;
}

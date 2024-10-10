window.focusElement = function (elementId) {
    var element = document.getElementById(elementId);
    if (element) {
        element.focus();
    }
}

window.downloadFile = function (fileName, fileContent) {
    var element = document.createElement('a');
    element.setAttribute('href', 'data:text/xml;charset=utf-8,' + encodeURIComponent(fileContent));
    element.setAttribute('download', fileName);
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
};

function toggleDropdown(containerId) {
    var element = document.getElementById(containerId);
    var showDropdown = "dropdown-show";
    var hideDropdown = "dropdown-hide";

    if (element.classList.contains(showDropdown)) {
        element.classList.remove(showDropdown);
        element.classList.add(hideDropdown);
    } else {
        element.classList.remove(hideDropdown);
        element.classList.add(showDropdown);
    }
}

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

window.downloadFileFromStream = async function (fileName, contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName;
    anchorElement.click();
    URL.revokeObjectURL(url);
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

function applyShakeEffect(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.classList.add("shake");

        // Entfernt die Klasse nach der Animation, sodass sie später wieder hinzugefügt werden kann
        element.addEventListener("animationend", () => {
            element.classList.remove("shake");
        }, { once: true });
    }
}

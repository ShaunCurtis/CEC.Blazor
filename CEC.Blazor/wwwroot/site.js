window.getDimensions = function () {
    return {
        width: window.innerWidth,
        height: window.innerHeight
    };
};

window.clearHistory = function () {
    window.history.forward();
    return true;
};

window.setFocus = function (id) {
    var el = document.getElementById(id);
    if (el) {
        el.focus();
        return true;
    }
    else return false;
}

window.setExitCheck = function (action) {
    if (action) {
        window.addEventListener("beforeunload", askExit);
    }
    else {
        window.removeEventListener("beforeunload", askExit);
    }
}

window.askExit = function (event) {
    event.preventDefault();
    event.returnValue = "There are unsaved changes on this page.  Do you want to leave?";
}

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

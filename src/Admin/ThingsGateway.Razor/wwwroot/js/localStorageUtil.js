// 设置 culture
function setCultureLocalStorage(culture) {
    localStorage.setItem("culture", culture);
}

// 获取 culture
function getCultureLocalStorage() {
    return localStorage.getItem("culture");
}

 function getLocalStorage(name) {
    return JSON.parse(localStorage.getItem(name)) ?? 0;
}
 function setLocalStorage(name, data) {
    if (localStorage) {
        localStorage.setItem(name, JSON.stringify(data));
    }
}

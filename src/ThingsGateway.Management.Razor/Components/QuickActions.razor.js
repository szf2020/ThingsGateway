export function toggle(id) {
    const el = document.getElementById(id)
    if (el === null) {
        return
    }
    const themeList = el.querySelector('.quickactions-list')
    //切换高度
    themeList.classList.toggle('is-open')
}
export function getAutoRestartThread() {
    return JSON.parse(localStorage.getItem('restart'))??true;
}

export function saveAutoRestartThread(restart) {
    if (localStorage) {
        localStorage.setItem('restart', JSON.stringify(restart));
    }
}
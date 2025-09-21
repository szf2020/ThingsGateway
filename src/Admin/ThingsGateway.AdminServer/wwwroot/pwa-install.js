let installPromptTriggered = false;

function getCookie(name) {
    const match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
    return match ? match[2] : null;
}

function hasShownInstallPrompt() {
    return getCookie("tgPWAInstallPromptShown") === "true";
}

function markInstallPromptShown() {
    document.cookie = "tgPWAInstallPromptShown=true; max-age=31536000; path=/";
}

window.addEventListener('beforeinstallprompt', (e) => {
    e.preventDefault();

    if (!hasShownInstallPrompt() && !installPromptTriggered) {
        installPromptTriggered = true;
        setTimeout(() => {
            e.prompt()
                .then(() => e.userChoice)
                .then(choiceResult => {
                    markInstallPromptShown();
                })
                .catch(err => {
                    // 可选错误处理
                });
        }, 2000); // 延迟 2 秒提示
    } else {
        // console.log("已提示过安装，不再弹出");
    }
});

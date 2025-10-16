let handlers = {};

export function init(id, invoke, options) {
    //function getCellByClass(row, className) {
    //    // 直接用 querySelector 精确查找
    //    return row.querySelector(`td.${className}`);
    //}
    var variableHandler = setInterval(async () => {
        var treeview = document.getElementById(id);
        if (!treeview) {
            clearInterval(variableHandler);
            return;
        }

        const spans = treeview.querySelectorAll(
            '.tree-content[style*="--bb-tree-view-level: 2"] .tree-node > span, ' +
            '.tree-content[style*="--bb-tree-view-level: 3"] .tree-node > span'
        );

        if (!spans) {
            return;
        }

        const ids = Array.from(spans).map(span => span.id);

        var { method } = options;

        if (!invoke) return;
        var valss = await invoke.invokeMethodAsync(method, ids);
        if (!valss || valss.length === 0) return;
        // 遍历 valss，下标 i 对应 span[i]
        for (let i = 0; i < valss.length && i < spans.length; i++) {
            const val = valss[i];
            const span = spans[i];

            if (!span) continue;

            if (span.className !== val) {
                span.className = val;
            }

        }

    }
        , 1000) //1000ms刷新一次

    handlers[id] = { variableHandler, invoke };

}
export function dispose(id) {
    const handler = handlers[id];
    if (handler) {
        clearInterval(handler.timer);
        handler.invoke = null;
        delete handlers[id];
    }
}

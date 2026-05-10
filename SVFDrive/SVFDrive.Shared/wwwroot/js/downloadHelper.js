window.svfPendingOps = 0;

window.svfBeginPending = function () { window.svfPendingOps++; };
window.svfEndPending = function () { window.svfPendingOps = Math.max(0, window.svfPendingOps - 1); };

window.addEventListener('beforeunload', function (e) {
    if (window.svfPendingOps > 0) {
        e.preventDefault();
        e.returnValue = '';
        return '';
    }
});

window.svfDownload = function (url) {
    const iframe = document.createElement('iframe');
    iframe.style.display = 'none';
    iframe.src = url;
    document.body.appendChild(iframe);

    setTimeout(function () {
        if (iframe && iframe.parentNode) {
            iframe.parentNode.removeChild(iframe);
        }
    }, 60000);
};

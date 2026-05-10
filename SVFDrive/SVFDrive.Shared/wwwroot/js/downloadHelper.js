window.svfDownload = function (url) {
    const iframe = document.createElement('iframe');
    iframe.style.display = 'none';
    iframe.src = url;
    document.body.appendChild(iframe);

    // Remove the iframe after a delay; the download itself is owned by the browser
    // and continues regardless. 60s is plenty for headers to arrive.
    setTimeout(function () {
        if (iframe && iframe.parentNode) {
            iframe.parentNode.removeChild(iframe);
        }
    }, 60000);
};

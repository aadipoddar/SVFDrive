window.svfPickAndUpload = function (apiBaseUrl, parentPath, dotnetRef, multiple) {
    const input = document.createElement('input');
    input.type = 'file';
    input.multiple = !!multiple;
    input.style.display = 'none';
    document.body.appendChild(input);

    input.addEventListener('change', async () => {
        const files = Array.from(input.files || []);
        document.body.removeChild(input);

        if (files.length === 0) {
            if (dotnetRef) await dotnetRef.invokeMethodAsync('OnUploadAllFinished', 0, 0);
            return;
        }

        let succeeded = 0, failed = 0;

        for (const file of files) {
            const url = apiBaseUrl + 'api/FileFolderManager/UploadFile'
                + '?parentPath=' + encodeURIComponent(parentPath)
                + '&name=' + encodeURIComponent(file.name)
                + '&overwrite=false';

            const ok = await new Promise((resolve) => {
                const xhr = new XMLHttpRequest();
                xhr.open('POST', url);
                xhr.upload.onprogress = (e) => {
                    if (e.lengthComputable && dotnetRef) {
                        dotnetRef.invokeMethodAsync('OnUploadProgress', file.name, e.loaded, e.total).catch(() => { });
                    }
                };
                xhr.onload = () => resolve(xhr.status >= 200 && xhr.status < 300);
                xhr.onerror = () => resolve(false);
                xhr.send(file);
            });

            if (ok) succeeded++; else failed++;

            if (dotnetRef) {
                await dotnetRef.invokeMethodAsync('OnUploadFileFinished', file.name, ok).catch(() => { });
            }
        }

        if (dotnetRef) await dotnetRef.invokeMethodAsync('OnUploadAllFinished', succeeded, failed);
    });

    input.click();
};

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

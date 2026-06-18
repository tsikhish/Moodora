(() => {
    const fallbackImage = "https://placehold.co/800x600?text=Image+preview";

    const normalizeImageUrl = (value) => {
        const url = value ? value.trim() : "";
        if (!url) return "";
        if (url.startsWith("http://") || url.startsWith("https://") || url.startsWith("/")) return url;
        return `https://${url}`;
    };
    const setPreview = (input) => {
        const targetId = input.dataset.preview;
        if (!targetId) return;

        const target = document.getElementById(targetId);
        if (!target) return;

        const url = normalizeImageUrl(input.value);

        if (!url) {
            target.innerHTML = "";
            target.classList.remove("is-broken");
            return;
        }

        const safeUrl = url.replace(/"/g, "&quot;");
        const safeFallback = fallbackImage.replace(/"/g, "&quot;");

        target.classList.remove("is-broken");
        target.innerHTML = `
            <img src="${safeUrl}"
                 alt="Image preview"
                 onerror="this.onerror=null; this.src='${safeFallback}'; this.parentElement.classList.add('is-broken');" />
        `;
    };

    document.querySelectorAll(".js-image-input").forEach((input) => {
        setPreview(input);
        input.addEventListener("input", () => setPreview(input));
    });
})();
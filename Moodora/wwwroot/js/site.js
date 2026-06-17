(() => {
    const setPreview = (input) => {
        const targetId = input.dataset.preview;
        if (!targetId) return;

        const target = document.getElementById(targetId);
        if (!target) return;

        const url = input.value ? input.value.trim() : "";

        if (!url) {
            target.innerHTML = "";
            target.classList.remove("is-broken");
            return;
        }

        const safeUrl = url.replace(/"/g, "&quot;");

        target.innerHTML = `
            <img src="${safeUrl}" 
                 alt="Image preview"
                 onerror="this.parentElement.classList.add('is-broken'); this.remove();" />
        `;
    };

    document.querySelectorAll(".js-image-input").forEach((input) => {
        setPreview(input);
        input.addEventListener("input", () => setPreview(input));
    });
})();
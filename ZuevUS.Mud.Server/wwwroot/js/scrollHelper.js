window.scrollHelper = {
    initialize: function (dotNetHelper) {
        window.addEventListener("scroll", () => {
            dotNetHelper.invokeMethodAsync("OnScroll", window.scrollY);
        });
    }
};
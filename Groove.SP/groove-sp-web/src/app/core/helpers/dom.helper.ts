
export class DomHelper {

    public static scrollToTop() {
        this.scrollToSmoothly(0, 500); // Scroll to top in 0.5s
    }

    private static scrollToSmoothly(pos, time) {
        if (typeof pos !== "number") {
            pos = parseFloat(pos);
        }
        if (isNaN(pos)) {
            throw "Position must be a number";
        }
        if (pos < 0 || time < 0) {
            return;
        }
        var currentPos = window.scrollY || window.screenTop;
        var start = null;
        time = time || 500;
        window.requestAnimationFrame(function step(currentTime) {
            start = !start ? currentTime : start;
            if (currentPos < pos) {
                var progress = currentTime - start;
                window.scrollTo(0, ((pos - currentPos) * progress / time) + currentPos);
                if (progress < time) {
                    window.requestAnimationFrame(step);
                } else {
                    window.scrollTo(0, pos);
                }
            } else {
                var progress = currentTime - start;
                window.scrollTo(0, currentPos - ((currentPos - pos) * progress / time));
                if (progress < time) {
                    window.requestAnimationFrame(step);
                } else {
                    window.scrollTo(0, pos);
                }
            }
        });
    }
}
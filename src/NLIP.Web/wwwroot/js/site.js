window.nlip = window.nlip || {};

(function () {
  const STORAGE_KEY = "nlip-theme";
  const stored = localStorage.getItem(STORAGE_KEY);
  if (stored) document.documentElement.setAttribute("data-theme", stored);

  window.nlip.toggleTheme = function () {
    const current = document.documentElement.getAttribute("data-theme") === "light" ? "light" : "dark";
    const next = current === "light" ? "dark" : "light";
    document.documentElement.setAttribute("data-theme", next);
    localStorage.setItem(STORAGE_KEY, next);
  };

  const charts = {};

  window.nlip.renderLineChart = function (canvasId, labels, data) {
    const canvas = document.getElementById(canvasId);
    if (!canvas || typeof Chart === "undefined") return;

    if (charts[canvasId]) charts[canvasId].destroy();

    charts[canvasId] = new Chart(canvas.getContext("2d"), {
      type: "line",
      data: {
        labels: labels,
        datasets: [{
          label: "Submissions",
          data: data,
          borderColor: "#4f7cff",
          backgroundColor: "rgba(79, 124, 255, 0.15)",
          fill: true,
          tension: 0.35,
          pointRadius: 2
        }]
      },
      options: {
        responsive: true,
        plugins: { legend: { display: false } },
        scales: { y: { beginAtZero: true } }
      }
    });
  };
})();

const formatarMoeda = (input) => {
    let valor = input.value.replace(/\D/g, "");

    if (valor === "") {
        input.value = "";
        return;
    }

    valor = (Number(valor) / 100).toFixed(2) + "";
    valor = valor.replace(".", ",");
    valor = valor.replace(/(\d)(?=(\d{3})+(?!\d))/g, "$1.");

    input.value = valor;
};

document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll('.money').forEach(input => {
        formatarMoeda(input);
        input.addEventListener('input', (e) => formatarMoeda(e.target));
    });
});

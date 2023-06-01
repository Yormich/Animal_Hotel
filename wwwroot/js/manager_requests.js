const btnSubmit = document.querySelector('.btn__submit');
const statusSelect = document.querySelector('.status__select');
const initialStatus = statusSelect.value;

for (let i = 0; i < statusSelect.length; i++) {
  if (Number.parseInt(statusSelect.options[i].value) === 1) {
    statusSelect.options[i].disabled = true;
    break;
  }
}

statusSelect.addEventListener('change', function () {
  if (Number.parseInt(statusSelect.value) !== Number.parseInt(initialStatus)) {
    btnSubmit.classList.remove('hidden');
    return;
  }
  btnSubmit.classList.add('hidden');
});

const roomSelect = document.querySelector('.form__input-select');
const roomTypeCheckBox = document.querySelector('.form_input-checkbox');

roomSelect.addEventListener('change', function () {
  console.log(roomSelect.value);
  console.log(roomTypeCheckBox.value);
  if (Number.parseInt(roomSelect.value) !== 1) {
    roomTypeCheckBox.checked = true;
    roomTypeCheckBox.value = true;
    return;
  }

  roomTypeCheckBox.disabled = false;
});

roomTypeCheckBox.addEventListener('change', function () {
  if (Number.parseInt(roomSelect.value) !== 1) {
    roomTypeCheckBox.checked = true;
    roomTypeCheckBox.value = true;
  }
});

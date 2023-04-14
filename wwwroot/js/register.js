const fileInput = document.getElementById('photo-input');
const fileLbl = document.querySelector('.for_input-file');

fileInput.addEventListener('change', function (e) {
  fileLbl.textContent = `Uploaded ${e.target.files[0].name}`;
});

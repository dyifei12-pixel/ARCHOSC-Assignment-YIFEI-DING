
const myName = document.querySelector(".name");
const myDescription = document.querySelector(".description");
console.log(myName);
console.log(myDescription);


const darkBtn = document.querySelector("#darkBtn");


darkBtn.addEventListener("click", function() {
    
    document.body.classList.toggle("dark");
})
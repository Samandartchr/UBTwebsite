//Import firebase
import { initializeApp } from "https://www.gstatic.com/firebasejs/10.13.1/firebase-app.js";
import { getAuth, onAuthStateChanged, signOut, createUserWithEmailAndPassword, 
        signInWithEmailAndPassword, sendEmailVerification } 
from "https://www.gstatic.com/firebasejs/10.13.1/firebase-auth.js";

//Firebase keys
const firebaseConfig = {
    apiKey: "AIzaSyAVZ-AchG-qwZFItO7E3LwEjONF1jI_Vw0",
    authDomain: "ubtwebsite.firebaseapp.com",
    projectId: "ubtwebsite",
    storageBucket: "ubtwebsite.firebasestorage.app",
    messagingSenderId: "389268561038",
    appId: "1:389268561038:web:453dc4313a67d82a553ddf",
    measurementId: "G-9BS8WWSGMZ"
  };

//Initialize firebase
const app = initializeApp(firebaseConfig);
export const auth = getAuth(app);

let user = null;
onAuthStateChanged(auth, (currentUser) => {
  user = currentUser;
});

document.getElementById("regBtn").addEventListener("click", register);
document.getElementById("loginBtn").addEventListener("click", login);
document.getElementById("createUserBtn").addEventListener("click", createUser);

//Register
async function register() 
{
  const email = document.getElementById("regEmail").value;
  const password = document.getElementById("regPassword").value;
  try 
  {
    const userCred = await createUserWithEmailAndPassword(auth, email, password);
    await sendEmailVerification(userCred.user);
    alert("Растау хаты электрондық поштаңызға жіберілді. Тіркелу үшін растаңыз.");
  } 
  catch (error) {
    alert(error.message);
  }
}
window.register = register;

//Login
async function login()
{
  const email = document.getElementById("loginEmail").value;
  const password = document.getElementById("loginPassword").value;
  try 
  {
    await signInWithEmailAndPassword(auth, email, password);

    user = auth.currentUser;

    if (!user) 
    {
      alert("Пайдаланушы табылмады.");
      return;
    }
    else if (!user.emailVerified) 
    {
      alert("Электрондық поштаңызды растаңыз.");
      //await signOut(auth);
    }

    //Custom API to check existense in database
    const exists = await fetch('http://localhost:5275/api/auth/checkuserexistence', {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${await getToken()}`,
        'Content-Type': 'application/json'
      },
    })

    const userExists = await exists.json();

    console.log(userExists);

    if (userExists == true)
    {
      window.location.href = "home.html";
    }

    else if (!userExists && user.emailVerified) 
    {
      // Redirect to registration form
      alert("Мәліметтерді толтырыңыз");
      document.getElementById('login').classList.add('hidden');
      document.getElementById('secondary').classList.remove('hidden');
    }
  } 
  catch (error) 
  {
    alert(error.message);
  }
}
window.login = login;

//Create user
async function createUser()
{
  var userregister = 
  {
    email: user.email,
    username: document.getElementById("regUsername").value,
    name: document.getElementById("regName").value,
    surname: document.getElementById("regSurname").value,
    role: document.getElementById("regRole").value
  }

  console.log(userregister);

  var token = await getToken();

  try
  {
    const response = await fetch('http://localhost:5275/api/auth/createuser', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(userregister)
    });

    if (response.ok) 
    {
      window.location.href = "home.html";
    } 
  }
  catch(error)
  {
    alert(error.message);
  }
}

//Logout
async function logout()
{

}
window.logout = logout;

//get token
async function getToken() {
  const user = auth.currentUser;
  if (!user) {
    console.log("No user signed in");
    return null;
  }
  
  const token = await user.getIdToken();
  return token;
}
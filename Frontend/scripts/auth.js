//Import firebase
import { initializeApp } from "https://www.gstatic.com/firebasejs/10.13.1/firebase-app.js";
import { getAuth, signOut, createUserWithEmailAndPassword, 
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
    alert("Кіру сәтті орындалды.");

    const user = auth.currentUser;

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
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ email: user.email })
    })

    if (!exists) 
    {
      // Redirect to registration form
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

//Logout
async function logout()
{

}
window.logout = logout;

//get token
export async function getToken()
{
  const user = auth.currentUser;
  if (user) 
  {
    return await user.getIdToken();
  }
  return null;
}
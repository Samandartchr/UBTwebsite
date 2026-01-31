//Import firebase
import { initializeApp } from "https://www.gstatic.com/firebasejs/10.13.1/firebase-app.js";
import { getAuth, onAuthStateChanged, signOut, createUserWithEmailAndPassword, 
        signInWithEmailAndPassword, sendEmailVerification } 
from "https://www.gstatic.com/firebasejs/10.13.1/firebase-auth.js";
import { signInWithCustomToken } from "https://www.gstatic.com/firebasejs/10.13.1/firebase-auth.js";

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

const usernameInput = document.getElementById("regUsername");
const usernameHint = document.getElementById("usernameHint");

const nameInput = document.getElementById("regName");
const nameHint = document.getElementById("nameHint");

const surnameInput = document.getElementById("regSurname");
const surnameHint = document.getElementById("surnameHint");

usernameInput.addEventListener("input", () => {
  const value = usernameInput.value.toLowerCase();
    usernameInput.value = value;

    const valid = /^[a-z0-9._]{4,15}$/.test(value);

    if (!value) {
        usernameHint.textContent = "";
        //usernameInput.className = "";
        return;
    }

    if (valid) {
        usernameInput.classList.add("valid");
        usernameInput.classList.remove("invalid");
        //usernameInput.className = "valid";
        usernameHint.textContent = "✓ valid";
    } else {
        usernameInput.classList.add("invalid");
        usernameInput.classList.remove("valid");
        //usernameInput.className = "invalid";
        usernameHint.textContent = "only a–z, 0–9, . _ (4–15 chars)";
    }
});

nameInput.addEventListener("input", () => {
  const value = nameInput.value.trim();

  if (!value) {
    nameHint.textContent = "";
    nameInput.classList.remove("valid", "invalid");
    return;
  }

  if (isUnicodeNameValid(value)) {
    nameInput.classList.add("valid");
    nameInput.classList.remove("invalid");
    nameHint.textContent = "✓ valid";
  } else {
    nameInput.classList.add("invalid");
    nameInput.classList.remove("valid");
    nameHint.textContent = "letters only";
  }
});

surnameInput.addEventListener("input", () => {
  const value = surnameInput.value.trim();

  if (!value) {
    surnameHint.textContent = "";
    surnameInput.classList.remove("valid", "invalid");
    return;
  }

  if (isUnicodeNameValid(value)) {
    surnameInput.classList.add("valid");
    surnameInput.classList.remove("invalid");
    surnameHint.textContent = "✓ valid";
  } else {
    surnameInput.classList.add("invalid");
    surnameInput.classList.remove("valid");
    surnameHint.textContent = "letters only";
  }
});


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
//Create user
async function createUser()
{
  // Validate username
  const result = validateUsername(document.getElementById("regUsername").value);
  if (!result.ok) 
  {
    alert(result.error);
    return;
  }
  document.getElementById("regUsername").value = result.value;

  // FIX: Validate name - isUnicodeNameValid returns boolean, not object!
  const nameValue = document.getElementById("regName").value.trim();
  if (!isUnicodeNameValid(nameValue)) 
  {
    alert("Аты дұрыс емес. Тек әріптер қолданыңыз.");
    return;
  }

  // FIX: Validate surname - same issue
  const surnameValue = document.getElementById("regSurname").value.trim();
  if (!isUnicodeNameValid(surnameValue)) 
  {
    alert("Тегі дұрыс емес. Тек әріптер қолданыңыз.");
    return;
  }

  var userregister = 
  {
    email: user.email,
    username: document.getElementById("regUsername").value,
    name: nameValue,  // Use validated values
    surname: surnameValue,  // Use validated values
    role: document.getElementById("regRole").value
  }

  console.log("Sending user data:", userregister);

  var token = await getToken();
  console.log("Token:", token ? "✓ Obtained" : "✗ Missing");

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

    console.log("Response status:", response.status);
    console.log("Response ok:", response.ok);

    if (response.ok) 
    {
      const data = await response.json();
      console.log("Backend returned:", data);

      if (data === true) 
      {
        console.log("✓ User created successfully! Redirecting...");
        window.location.href = "home.html";
      } 
      else 
      {
        console.error("✗ Backend returned false");
        alert("Тіркеу сәтсіз аяқталды");
      }
    }
    else
    {
      const err = await response.json();
      console.error("Server error:", err);
      alert(err.message || err.error || "Сервер қатесі");
    }
  }
  catch(error)
  {
    console.error("Fetch error:", error);
    alert(`Қате: ${error.message}`);
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

function validateUsername(username) {
    if (!username || !username.trim()) return { ok: false, error: "Username required" };

    const normalized = username.toLowerCase();

    const regex = /^[a-z0-9._]+$/;
    if (!regex.test(normalized)) {
        return {
            ok: false,
            error: "Only lowercase letters, numbers, . and _ allowed"
        };
    }

    return { ok: true, value: normalized };
}

function isUnicodeNameValid(value) {
  return /^[\p{L}]+$/u.test(value);
}

async function loginWithToken(customToken) {
  try {
    const userCredential = await signInWithCustomToken(auth, customToken);
    console.log("User signed in:", userCredential.user);
    return userCredential.user;
  } catch (error) {
    console.error("Error signing in with token:", error);
    throw error;
  }
}
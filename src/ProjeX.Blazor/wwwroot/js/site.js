// Site-wide JavaScript functions

window.submitLoginForm = (email, password, rememberMe) => {
    // Create a form dynamically and submit it
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = '/Account/Login';
    
    // Add CSRF token
    const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]');
 if (csrfToken) {
        const tokenInput = document.createElement('input');
        tokenInput.type = 'hidden';
      tokenInput.name = '__RequestVerificationToken';
        tokenInput.value = csrfToken.value;
        form.appendChild(tokenInput);
    }
    
    // Add email
    const emailInput = document.createElement('input');
    emailInput.type = 'hidden';
    emailInput.name = 'Email';
    emailInput.value = email;
    form.appendChild(emailInput);
    
    // Add password
    const passwordInput = document.createElement('input');
 passwordInput.type = 'hidden';
    passwordInput.name = 'Password';
    passwordInput.value = password;
    form.appendChild(passwordInput);
    
    // Add remember me
    if (rememberMe) {
      const rememberInput = document.createElement('input');
        rememberInput.type = 'hidden';
        rememberInput.name = 'RememberMe';
     rememberInput.value = 'true';
     form.appendChild(rememberInput);
    }
    
    document.body.appendChild(form);
    form.submit();
};
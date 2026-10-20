const LOGO={full:"assets/nexacore-full.png",mark:"assets/nexacore-mark.png"};
(()=>{const l=document.createElement("link");l.rel="icon";l.href=LOGO.mark;document.head.append(l);addEventListener("DOMContentLoaded",()=>document.querySelectorAll("img[data-logo]").forEach(i=>i.src=LOGO[i.dataset.logo]))})();

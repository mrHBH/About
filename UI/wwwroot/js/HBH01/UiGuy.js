


export const Ui_Guy = (() => {

  class Ui_Guy {
    constructor(params) {
     // this._params = params;
      this._cli=params.cli._cli;
      this._CsharpRefrence=params.csharp;
      this.InitComponent();
    }

    InitComponent() {
      
      this._LoadUI2();
       
    }
    _LoadUI1() {

      let ActiveFrames = document.getElementById("ActiveFrames"); 
      let g = document.createElement('li');
      g.setAttribute("class", "slider__frame glide__slide");
      let div = document.createElement('img');
      div.setAttribute("src", "https://source.unsplash.com/1600x900/?dna");
      g.appendChild(div);
      ActiveFrames.appendChild(g);
    
     
      this.UpdateGlider();

      // document.getElementById('cliContainer').addEventListener('keydown', event => {

      //   this.Attention = "Cli";
      //   this._input._resetAll();
      //   if (this._cli) {
      //     if (event.key === "Enter") {
      //       this._cli.enterKey();
      //       this._cli.println("");
      //       return;
      //     }
      //     this._cli.type(event.key);
      //     // this._cli.printPrompt();
      //     //   alert(event.key);
      //   }
      // })

       document.getElementById('c').addEventListener('click', event => {

       this.Attention = "Canvas";

         this._cli.type("Attention : " + this.Attention)
         this._cli.println("");
         this._cli.printPrompt();
         this._cli.printCursor();
        //this._input._resetAll();
       })

      document.getElementById('controls').addEventListener('mousedown', event => {
        this.Remount2();

        this.Attention = "Controls";
        this._cli.type("Attention : " + this.Attention);
        this._cli.println("");
        this._cli.printPrompt();
        this._cli.printCursor();
    //    this._input._resetAll();
      })
      var userSelection = document.getElementsByClassName('gutter gutter-horizontal');
       for (var i = 0; i < userSelection.length; i++) {
         (function (index) {
           userSelection[index].addEventListener("mouseup", function () {

       this.Attention = "Gutter";
       this._cli.type("Attention : " + this.Attention)
       this._cli.println();
       this._cli.printPrompt();
       this._cli.printCursor();   
           })
       })(i);
       }
    

    }

    _LoadUI2() {

      let ActiveFrames = document.getElementById("ActiveFrames2"); 
      let g = document.createElement('li');
      g.setAttribute("class", "slider__frame glide__slide");
      let div = document.createElement('img');
      div.setAttribute("src", "https://source.unsplash.com/1600x900/?Dresden");
      g.appendChild(div);
      ActiveFrames.appendChild(g); 
     
      this.UpdateGlider();
      document.getElementById('c').addEventListener('click', event => {

        this.Attention = "Canvas";
 
          this._cli.type("Attention : " + this.Attention)
          this._cli.println("");
          this._cli.printPrompt();
          this._cli.printCursor();
         //this._input._resetAll();
        })
 
       document.getElementById('controls').addEventListener('mousedown', event => {
         this.Remount2();
 
         this.Attention = "Controls";
         this._cli.type("Attention : " + this.Attention);
         this._cli.println("");
         this._cli.printPrompt();
         this._cli.printCursor();
     //    this._input._resetAll();
       })
       var userSelection = document.getElementsByClassName('gutter gutter-horizontal');
        for (var i = 0; i < userSelection.length; i++) {
          (function (index) {
            userSelection[index].addEventListener("mouseup", function () {
 
        this.Attention = "Gutter";
        this._cli.type("Attention : " + this.Attention)
        this._cli.println();
        this._cli.printPrompt();
        this._cli.printCursor();   
            })
        })(i);
        }
     
 
     
     
    

    }
 

    UpdateGlider() {


      // add content 
      if (this.glideHero)
      {

        this.glideHero.destroy();
      }

      this.glideHero = new Glide('.glide', {
        type: "carousel",
        touchAngle: 45,
        focusAt: 1,
        startAt: 1,
        perView: 1,
        keyboard: false,
        gap: 5,
        autoplay: false,
        peek: {
          before: 100,
          after: 50
        },

      })
      this.glideHero.mount();

      // this.glideHero.on(['mount.after', 'run'], function () {
      //   
      //         alert("gello" +  this.glideHero.index );
      //     })
      //  this.glideHero.mount();
      //this.glideHero.go('<');


    }


    Remount2 = function(){
      this.glideHero.update();
     // this.glideHero.mount();
      }


    Remount() {
      //   if (this.Isgliding){
      //  this.UpdateGlider();
      // this.glideHero.mount();
      //  // this.glideHero.mount();
      // }
      //this.glideHero.mount();
      this.glideHero.update();
    }


    Update(timeInSeconds) {
      //   if (this.Isgliding){
      //  this.UpdateGlider();
      //  // this.glideHero.mount();
      // }
    }
  };
 
  return {
    Ui_Guy: Ui_Guy,
  };

})();
int y = 0;

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  
}

void loop() {
  // put your main code here, to run repeatedly:
  if(Serial.available() > 0){
    y = Serial.parseInt();// ler inteiros e armazena na variavel y
    Serial.println(y);
  }
  
}

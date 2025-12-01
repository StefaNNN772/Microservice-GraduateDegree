import {JwtPayload} from "jwt-decode";

export interface CustomJwtPayload extends JwtPayload {
  id: string;
  sub: string;
  role: string;
  exp: number;
}

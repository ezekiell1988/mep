import { Grupo } from '../api/endpoints';

export type RootStackParamList = {
  Grupos: undefined;
  Estudiantes: { grupo: Grupo };
  TomarLista: { grupo: Grupo; date: string };
  Planeamientos: undefined;
  PlaneamientoDetalle: { planId: string; asignatura: string; nivel: number; trimestre: number };
};

/*
Copyright 2011 Arran Schlosberg (http://arranschlosberg.com);

This file is part of CSharp-ChaoticCipher (https://github.com/aschlosberg/CSharp-ChaoticCipher).

    CSharp-ChaoticCipher is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    CSharp-ChaoticCipher is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with CSharp-ChaoticCipher.  If not, see <http://www.gnu.org/licenses/>.

---------------------------------------------------------------------------------------
*/

using System;
namespace ChaoticCipher
{
	public class Ball : NonLinearSystem
	{
		/*
		 * See http://en.wikipedia.org/wiki/Bouncing_ball_dynamics
		 * Implemented in 1 dimension
		 * 
		 * b = ball
		 * t = table
		 * x = coordinate
		 * v = velocity
		 * g = gravity
		 * h = max height in metres
		 */
		long xb, xt, vb, vt, g, h=(long)Math.Pow(2, 55), metre;
		
		//sinusoidal amplitude and Hooke's law constant for table
		ushort A;
		decimal hooke = 0.21m, 
				air = 0.1m;
		
		public Ball ()
		{
			metre = long.MaxValue / h;
			g = (long) Math.Floor(9.81 * metre);
			
			A = (ushort) (metre * 2048);
			
			//table is at a peak -> static
			xt = A;
			vt = 0;
			
			xb = A * 4;
			vb = 0;
		}
		
		public override void Iterate(ushort nudge)
		{
			nudge = (ushort) ((nudge - 127) * A / 256); //nudge is (ushort) byte so centralise it and then scale to be more significant wrt A
			
			xt += nudge; 
			BoundTable();
			
			vt -= (long) (hooke * xt);
			xt += vt;
			
			vb += nudge - g;
			vb = (long) (vb * (1-air));
			long space = xb < 0 ? long.MaxValue : long.MaxValue - xb;

			if(space < vb){ //hit the roof
				xb = long.MaxValue - (vb-space);
				vb = -vb; //no dissapation and roof is infinitely heavy
			}
			else {
				xb += vb;
				if(xb < xt){ //hit the table
					xb += (xt - xb);
					vb = -vb + vt; //no dissapation and table is infinitely heavy
				}
			}
		}
		
		public override ulong RandomSource {
			get { return (ulong) Math.Abs(xb); }
		}
		
		private void BoundTable(){
			xt = (long) Math.Max(-A, Math.Min(A, xt));
		}
	}
}


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
	public class WaterWheel : NonLinearSystem
	{
		private int velocity = 0;
		private ulong angle = 0, spacing;
		private Bucket[] buckets = new Bucket[8];
		private const Decimal friction = 0.1m;
		
		public WaterWheel ()
		{
			this.spacing = ulong.MaxValue / (ulong) this.buckets.Length;
			
			for(ushort i=0; i<this.buckets.Length; i++){
				this.buckets[i] = new Bucket(i%3==0 ? 0 : (uint)Math.Pow(2, 16), i);
			}
		}
		
		public override void Iterate(ushort vol){
			ulong bAngle;
			int force = 0;
			
			foreach(Bucket b in this.buckets){
				b.Drip();
				bAngle = this.angle + this.spacing * b.GetIndex(); //will wrap to 0 automatically
				
				int dForce = (int) (Math.Cos((Double)(bAngle / ulong.MaxValue * 2 * Math.PI)) * b.GetFill());
				force = (int) Math.Max(int.MinValue, Math.Min(int.MaxValue, force+dForce));
			}
		
			this.velocity = (int) Math.Max(int.MinValue, Math.Min(int.MaxValue, this.velocity+force));
			this.angle += (uint) this.velocity; //deliberately not bounded
			
			this.velocity = (int) (this.velocity * (1-friction));
			
			ushort toFill = (ushort) Math.Floor((Decimal) this.angle/this.spacing);
			this.buckets[toFill].Fill(vol);
		}
		
		public override ulong RandomSource {
			get { return this.angle; }
		}
		
		public class Bucket {
			private ulong fill;
			private ushort index;
			private const Decimal dripRate = 0.1m;
			
			public Bucket(ulong fill, ushort index)
			{
				this.fill = fill;
				this.index = index;
			}
				
			public void Drip()
			{
				this.fill = (ulong) (this.fill * (1-dripRate));
			}
				
			public void Fill(ulong vol)
			{
				this.fill = (ulong) Math.Min(this.fill+vol, ulong.MaxValue);
			}
			
			public ulong GetFill()
			{
				return this.fill;
			}
			
			public ushort GetIndex()
			{
				return this.index;
			}
		}
	}
}